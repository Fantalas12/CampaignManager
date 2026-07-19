import React, { useState, useEffect } from 'react';
import { Note, Participant, Role, NoteAccess } from '../backend';
import { useGetNoteAccess, useGetUserProfile } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import ManageNoteAccessDialog from './ManageNoteAccessDialog';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { FileText, Calendar, Clock, Settings, Crown, Gamepad2, Eye, Lock, Tag } from 'lucide-react';
import { NOTE_PERMISSIONS, hasPermission } from '../lib/notePermissions';

interface NoteCardProps {
  note: Note;
  creatorName?: string;
  campaignParticipants: Array<Participant & { name?: string }>;
  currentUserRole?: Role;
  onNoteClick?: (note: Note) => void;
}

export default function NoteCard({ note, creatorName, campaignParticipants, currentUserRole, onNoteClick }: NoteCardProps) {
  const [isAccessDialogOpen, setIsAccessDialogOpen] = useState(false);
  const [userPermissions, setUserPermissions] = useState<bigint>(0n);
  const [loadingPermissions, setLoadingPermissions] = useState(false);
  
  const getNoteAccess = useGetNoteAccess();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();
  const isNoteOwner = note.creator.toString() === currentUserPrincipal?.toString();

  useEffect(() => {
    if (currentUserPrincipal && !isNoteOwner) {
      loadUserPermissions();
    }
  }, [note.id, currentUserPrincipal, isNoteOwner]);

  const loadUserPermissions = async () => {
    if (!currentUserPrincipal) return;
    
    setLoadingPermissions(true);
    try {
      const accessList = await getNoteAccess.mutateAsync(note.id);
      const userAccess = accessList.find(access => 
        access.participant.toString() === currentUserPrincipal.toString()
      );
      // Convert number to bigint for consistency with permission system
      setUserPermissions(userAccess ? BigInt(userAccess.permissions) : 0n);
    } catch (error) {
      console.error('Failed to load user permissions:', error);
      setUserPermissions(0n);
    } finally {
      setLoadingPermissions(false);
    }
  };

  const handleAccessUpdated = () => {
    loadUserPermissions();
  };

  const handleCardClick = (e: React.MouseEvent) => {
    // Don't trigger card click if clicking on buttons
    if ((e.target as HTMLElement).closest('button')) {
      return;
    }
    onNoteClick?.(note);
  };

  // Determine what content the user can see
  const canSeePlayerView = isNoteOwner || hasPermission(userPermissions, NOTE_PERMISSIONS.PLAYER_READ);
  const canSeeGMView = isNoteOwner || hasPermission(userPermissions, NOTE_PERMISSIONS.GM_READ);
  const canSeeAnyContent = isNoteOwner || canSeePlayerView || canSeeGMView;

  // If user has no read permissions, show restricted view
  if (!canSeeAnyContent && !loadingPermissions) {
    return (
      <Card 
        className="bg-white/5 backdrop-blur-sm border-white/10 opacity-60 cursor-pointer"
        onClick={handleCardClick}
      >
        <CardHeader className="pb-3">
          <div className="flex items-start justify-between">
            <div className="flex items-center space-x-2 flex-1 min-w-0">
              <Lock className="h-5 w-5 text-red-400 flex-shrink-0" />
              <div className="min-w-0 flex-1">
                <CardTitle className="text-white text-lg truncate">{note.name}</CardTitle>
                <div className="flex items-center space-x-4 mt-1">
                  <div className="flex items-center space-x-1 text-sm text-purple-300">
                    <span>by {creatorName || 'Unknown GM'}</span>
                  </div>
                  {note.inGameDateTime && (
                    <div className="flex items-center space-x-1 text-sm text-purple-300">
                      <Clock className="h-4 w-4" />
                      <span>{note.inGameDateTime}</span>
                    </div>
                  )}
                </div>
              </div>
            </div>
            <Badge variant="outline" className="text-xs text-red-300 border-red-400/30 flex-shrink-0">
              Restricted
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <CardDescription className="text-purple-200 italic">
            You don't have permission to view this note's content.
          </CardDescription>
        </CardContent>
      </Card>
    );
  }

  return (
    <>
      <Card 
        className="bg-white/5 backdrop-blur-sm border-white/10 hover:bg-white/10 transition-all duration-200 group cursor-pointer"
        onClick={handleCardClick}
      >
        <CardHeader className="pb-3">
          <div className="flex items-start justify-between">
            <div className="flex items-center space-x-2 flex-1 min-w-0">
              <FileText className="h-5 w-5 text-green-400 flex-shrink-0" />
              <div className="min-w-0 flex-1">
                <CardTitle className="text-white text-lg truncate">{note.name}</CardTitle>
                <div className="flex items-center space-x-4 mt-1">
                  <div className="flex items-center space-x-1 text-sm text-purple-300">
                    <span>by {creatorName || 'Unknown GM'}</span>
                  </div>
                  {note.inGameDateTime && (
                    <div className="flex items-center space-x-1 text-sm text-purple-300">
                      <Clock className="h-4 w-4" />
                      <span>{note.inGameDateTime}</span>
                    </div>
                  )}
                </div>
              </div>
            </div>
            <div className="flex items-center space-x-2 flex-shrink-0">
              <Badge variant="outline" className="text-xs text-green-300 border-green-400/30">
                Note
              </Badge>
              {isNoteOwner && (
                <Button
                  onClick={() => setIsAccessDialogOpen(true)}
                  variant="ghost"
                  size="sm"
                  className="h-8 px-2 text-purple-300 hover:text-white hover:bg-white/10 opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  <Settings className="h-4 w-4 mr-1" />
                  <span className="text-xs">Access</span>
                </Button>
              )}
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-3">
          {/* Display tags */}
          {note.tags.length > 0 && (
            <div className="flex flex-wrap gap-1">
              {note.tags.map((tag) => (
                <Badge
                  key={tag}
                  variant="secondary"
                  className="text-xs bg-purple-500/20 text-purple-200 border-purple-400/30 hover:bg-purple-500/30"
                >
                  <Tag className="h-3 w-3 mr-1" />
                  {tag}
                </Badge>
              ))}
            </div>
          )}

          {/* Show access level indicators for non-owners */}
          {!isNoteOwner && (canSeePlayerView || canSeeGMView) && (
            <div className="pt-2 border-t border-white/10">
              <div className="flex items-center space-x-2">
                <span className="text-xs text-purple-300">Access:</span>
                {canSeePlayerView && (
                  <Badge variant="outline" className="text-xs text-blue-300 border-blue-400/30">
                    <Gamepad2 className="h-3 w-3 mr-1" />
                    Player
                  </Badge>
                )}
                {canSeeGMView && (
                  <Badge variant="outline" className="text-xs text-amber-300 border-amber-400/30">
                    <Crown className="h-3 w-3 mr-1" />
                    GM
                  </Badge>
                )}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Access Management Dialog */}
      {isNoteOwner && (
        <ManageNoteAccessDialog
          note={note}
          campaignParticipants={campaignParticipants}
          open={isAccessDialogOpen}
          onOpenChange={setIsAccessDialogOpen}
          onAccessUpdated={handleAccessUpdated}
        />
      )}
    </>
  );
}
