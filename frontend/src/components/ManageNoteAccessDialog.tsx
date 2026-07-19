import React, { useState, useEffect } from 'react';
import { Note, Participant, Role, NoteAccess } from '../backend';
import { useManageNoteAccess, useGetNoteAccess, useGetUserProfile } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Badge } from '@/components/ui/badge';
import { Settings, Crown, Gamepad2, Loader2, Save, Shield, Eye, Edit, Play } from 'lucide-react';
import { Principal } from '@dfinity/principal';
import { NOTE_PERMISSIONS, hasPermission, togglePermission, getPermissionLabel, getPermissionDescription } from '../lib/notePermissions';

interface ManageNoteAccessDialogProps {
  note: Note;
  campaignParticipants: Array<Participant & { name?: string }>;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onAccessUpdated?: () => void;
}

interface ParticipantAccess extends Participant {
  name?: string;
  permissions: bigint;
  hasChanges: boolean;
}

export default function ManageNoteAccessDialog({ 
  note, 
  campaignParticipants, 
  open, 
  onOpenChange, 
  onAccessUpdated 
}: ManageNoteAccessDialogProps) {
  const [participantAccess, setParticipantAccess] = useState<ParticipantAccess[]>([]);
  const [loadingAccess, setLoadingAccess] = useState(false);
  const [savingAccess, setSavingAccess] = useState(false);
  
  const getNoteAccess = useGetNoteAccess();
  const manageNoteAccess = useManageNoteAccess();
  const getUserProfile = useGetUserProfile();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();
  const isNoteOwner = note.creator.toString() === currentUserPrincipal?.toString();

  // Filter out the note owner from the campaign participants
  const eligibleParticipants = campaignParticipants.filter(participant => 
    participant.principal.toString() !== note.creator.toString()
  );

  useEffect(() => {
    if (open && isNoteOwner) {
      loadNoteAccess();
    }
  }, [open, note.id, isNoteOwner]);

  const loadNoteAccess = async () => {
    setLoadingAccess(true);
    try {
      const accessList = await getNoteAccess.mutateAsync(note.id);
      
      // Create a map of participant permissions (convert number to bigint)
      const accessMap = new Map<string, bigint>();
      accessList.forEach(access => {
        accessMap.set(access.participant.toString(), BigInt(access.permissions));
      });

      // Load participant profiles and combine with access data (excluding note owner)
      const participantsWithAccess = await Promise.all(
        eligibleParticipants.map(async (participant) => {
          try {
            const profile = await getUserProfile.mutateAsync(participant.principal);
            return {
              ...participant,
              name: profile?.name,
              permissions: accessMap.get(participant.principal.toString()) || 0n,
              hasChanges: false,
            };
          } catch {
            return {
              ...participant,
              permissions: accessMap.get(participant.principal.toString()) || 0n,
              hasChanges: false,
            };
          }
        })
      );

      setParticipantAccess(participantsWithAccess);
    } catch (error) {
      console.error('Failed to load note access:', error);
    } finally {
      setLoadingAccess(false);
    }
  };

  const handlePermissionToggle = (participantPrincipal: Principal, permission: bigint) => {
    setParticipantAccess(prev => 
      prev.map(p => {
        if (p.principal.toString() === participantPrincipal.toString()) {
          const newPermissions = togglePermission(p.permissions, permission);
          return {
            ...p,
            permissions: newPermissions,
            hasChanges: true,
          };
        }
        return p;
      })
    );
  };

  const handleSaveChanges = async () => {
    setSavingAccess(true);
    try {
      const changedParticipants = participantAccess.filter(p => p.hasChanges);
      
      for (const participant of changedParticipants) {
        await manageNoteAccess.mutateAsync({
          noteId: note.id,
          participant: participant.principal,
          permissions: participant.permissions,
        });
      }

      // Reset change flags
      setParticipantAccess(prev => 
        prev.map(p => ({ ...p, hasChanges: false }))
      );

      onAccessUpdated?.();
    } catch (error) {
      console.error('Failed to save note access changes:', error);
    } finally {
      setSavingAccess(false);
    }
  };

  const getRoleIcon = (role: Role) => {
    switch (role) {
      case Role.gm:
        return <Crown className="h-4 w-4 text-amber-400" />;
      case Role.player:
        return <Gamepad2 className="h-4 w-4 text-blue-400" />;
      case Role.both:
        return (
          <div className="flex items-center space-x-1">
            <Crown className="h-3 w-3 text-amber-400" />
            <Gamepad2 className="h-3 w-3 text-blue-400" />
          </div>
        );
    }
  };

  const getPermissionIcon = (permission: bigint) => {
    switch (permission) {
      case NOTE_PERMISSIONS.PLAYER_READ:
        return <Eye className="h-3 w-3 text-blue-400" />;
      case NOTE_PERMISSIONS.GM_READ:
        return <Eye className="h-3 w-3 text-amber-400" />;
      case NOTE_PERMISSIONS.WRITE:
        return <Edit className="h-3 w-3 text-green-400" />;
      case NOTE_PERMISSIONS.EXECUTE:
        return <Play className="h-3 w-3 text-purple-400" />;
      default:
        return <Shield className="h-3 w-3 text-gray-400" />;
    }
  };

  const hasAnyChanges = participantAccess.some(p => p.hasChanges);

  if (!isNoteOwner) {
    return null;
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="bg-gray-900 border-white/20 text-white max-w-6xl max-h-[80vh]">
        <DialogHeader className="pb-4">
          <DialogTitle className="flex items-center gap-2 text-xl">
            <Settings className="h-5 w-5 text-green-400" />
            Manage Note Access
          </DialogTitle>
          <DialogDescription className="text-purple-200">
            Control who can access "{note.name}" and what they can do with it. As the note owner, you automatically have full permissions.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold text-white">
              Participant Permissions ({participantAccess.length})
            </h3>
            {hasAnyChanges && (
              <Button
                onClick={handleSaveChanges}
                disabled={savingAccess}
                className="bg-gradient-to-r from-green-600 to-emerald-600 hover:from-green-700 hover:to-emerald-700 text-white border-0"
              >
                {savingAccess ? (
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                ) : (
                  <Save className="h-4 w-4 mr-2" />
                )}
                Save Changes
              </Button>
            )}
          </div>

          <Separator className="bg-white/20" />

          <ScrollArea className="h-[500px] pr-4">
            {loadingAccess ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="h-8 w-8 animate-spin text-purple-400" />
                <span className="ml-2 text-purple-200">Loading access permissions...</span>
              </div>
            ) : participantAccess.length === 0 ? (
              <div className="text-center py-8">
                <Settings className="h-12 w-12 text-purple-400 mx-auto mb-4" />
                <p className="text-purple-200 mb-2">No other participants found</p>
                <p className="text-sm text-purple-300">Add participants to the campaign to manage note access. As the note owner, you automatically have full permissions.</p>
              </div>
            ) : (
              <div className="space-y-6">
                {participantAccess.map((participant) => (
                  <div
                    key={participant.principal.toString()}
                    className={`p-6 rounded-lg border transition-colors ${
                      participant.hasChanges 
                        ? 'bg-green-500/10 border-green-400/30' 
                        : 'bg-white/5 border-white/10 hover:bg-white/10'
                    }`}
                  >
                    <div className="flex items-start justify-between mb-6">
                      <div className="flex items-center space-x-3">
                        {getRoleIcon(participant.role)}
                        <div>
                          <p className="font-medium text-white text-lg">
                            {participant.name || 'Unknown User'}
                            {participant.hasChanges && (
                              <Badge variant="outline" className="ml-2 text-xs text-green-300 border-green-400/30">
                                Modified
                              </Badge>
                            )}
                          </p>
                          <p className="text-sm text-purple-300">
                            {participant.principal.toString().slice(0, 20)}...
                          </p>
                        </div>
                      </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      {Object.entries(NOTE_PERMISSIONS).map(([key, permission]) => (
                        <div
                          key={key}
                          className="flex items-start space-x-3 p-4 rounded-lg border border-white/10 hover:bg-white/5 transition-colors min-h-[80px]"
                        >
                          <Checkbox
                            id={`${participant.principal.toString()}-${key}`}
                            checked={hasPermission(participant.permissions, permission)}
                            onCheckedChange={() => handlePermissionToggle(participant.principal, permission)}
                            className="data-[state=checked]:bg-green-600 data-[state=checked]:border-green-600 mt-1 flex-shrink-0"
                          />
                          <div className="flex items-start space-x-3 flex-1 min-w-0">
                            <div className="mt-1 flex-shrink-0">
                              {getPermissionIcon(permission)}
                            </div>
                            <div className="min-w-0 flex-1">
                              <label
                                htmlFor={`${participant.principal.toString()}-${key}`}
                                className="text-sm font-medium text-white cursor-pointer block leading-5"
                              >
                                {getPermissionLabel(permission)}
                              </label>
                              <p className="text-xs text-purple-300 mt-1 leading-4">
                                {getPermissionDescription(permission)}
                              </p>
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </ScrollArea>

          {hasAnyChanges && (
            <div className="bg-amber-500/10 border border-amber-400/20 rounded-lg p-3">
              <p className="text-amber-300 text-sm">
                You have unsaved changes. Click "Save Changes" to apply the new permissions.
              </p>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
