import React, { useState, useEffect } from 'react';
import { Campaign, Participant, Role } from '../backend';
import { useGetParticipants, useRemoveParticipant, useGetUserProfile, useUpdateParticipantRole, useLeaveCampaign } from '../hooks/useQueries';
import SendInvitationDialog from './SendInvitationDialog';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { Users, UserPlus, Crown, Gamepad2, UserX, Loader2, Eye, AlertTriangle, Settings, LogOut } from 'lucide-react';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import { Principal } from '@dfinity/principal';

interface ParticipantsDialogProps {
  campaign: Campaign;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  isOwner?: boolean;
  onParticipantsUpdated?: () => void;
}

interface ParticipantWithProfile extends Participant {
  name?: string;
}

export default function ParticipantsDialog({ campaign, open, onOpenChange, isOwner = true, onParticipantsUpdated }: ParticipantsDialogProps) {
  const [participants, setParticipants] = useState<ParticipantWithProfile[]>([]);
  const [isInviteDialogOpen, setIsInviteDialogOpen] = useState(false);
  const [loadingParticipants, setLoadingParticipants] = useState(false);
  const [isRemoveConfirmOpen, setIsRemoveConfirmOpen] = useState(false);
  const [isLeaveConfirmOpen, setIsLeaveConfirmOpen] = useState(false);
  const [participantToRemove, setParticipantToRemove] = useState<ParticipantWithProfile | null>(null);
  const getParticipants = useGetParticipants();
  const removeParticipant = useRemoveParticipant();
  const updateParticipantRole = useUpdateParticipantRole();
  const leaveCampaign = useLeaveCampaign();
  const getUserProfile = useGetUserProfile();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();

  useEffect(() => {
    if (open) {
      loadParticipants();
    }
  }, [open]);

  const loadParticipants = async () => {
    setLoadingParticipants(true);
    try {
      const participantList = await getParticipants.mutateAsync(campaign.id);
      const participantsWithProfiles = await Promise.all(
        participantList.map(async (participant) => {
          try {
            const profile = await getUserProfile.mutateAsync(participant.principal);
            return {
              ...participant,
              name: profile?.name,
            };
          } catch {
            return participant;
          }
        })
      );
      setParticipants(participantsWithProfiles);
    } catch (error) {
      console.error('Failed to load participants:', error);
    } finally {
      setLoadingParticipants(false);
    }
  };

  const handleRemoveParticipantClick = (participant: ParticipantWithProfile) => {
    // Prevent removal if not authorized
    if (!canRemove(participant.principal)) {
      return;
    }
    
    setParticipantToRemove(participant);
    setIsRemoveConfirmOpen(true);
  };

  const handleConfirmRemoveParticipant = async () => {
    if (!participantToRemove) return;

    try {
      await removeParticipant.mutateAsync({
        campaignId: campaign.id,
        participant: participantToRemove.principal,
      });
      await loadParticipants();
      // Notify parent component that participants were updated
      onParticipantsUpdated?.();
      setIsRemoveConfirmOpen(false);
      setParticipantToRemove(null);
    } catch (error) {
      console.error('Failed to remove participant:', error);
      // Keep the dialog open on error so user can try again
    }
  };

  const handleCancelRemoveParticipant = () => {
    setIsRemoveConfirmOpen(false);
    setParticipantToRemove(null);
  };

  const handleLeaveCampaignClick = () => {
    setIsLeaveConfirmOpen(true);
  };

  const handleConfirmLeaveCampaign = async () => {
    try {
      await leaveCampaign.mutateAsync(campaign.id);
      setIsLeaveConfirmOpen(false);
      onOpenChange(false);
    } catch (error) {
      console.error('Failed to leave campaign:', error);
      // Keep the dialog open on error so user can try again
    }
  };

  const handleCancelLeaveCampaign = () => {
    setIsLeaveConfirmOpen(false);
  };

  const handleRoleChange = async (participant: ParticipantWithProfile, newRole: Role) => {
    if (!canEditRole(participant.principal)) {
      return;
    }

    try {
      await updateParticipantRole.mutateAsync({
        campaignId: campaign.id,
        participant: participant.principal,
        newRole,
      });
      await loadParticipants();
      // Notify parent component that participants were updated
      onParticipantsUpdated?.();
    } catch (error) {
      console.error('Failed to update participant role:', error);
    }
  };

  const handleInviteSent = async () => {
    await loadParticipants();
    // Notify parent component that participants were updated
    onParticipantsUpdated?.();
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

  const getRoleText = (role: Role) => {
    switch (role) {
      case Role.gm:
        return 'GM';
      case Role.player:
        return 'Player';
      case Role.both:
        return 'GM & Player';
    }
  };

  const getRoleBadgeVariant = (role: Role) => {
    switch (role) {
      case Role.gm:
        return 'default' as const;
      case Role.player:
        return 'secondary' as const;
      case Role.both:
        return 'outline' as const;
    }
  };

  const isOwnerParticipant = (participantPrincipal: Principal) => {
    return participantPrincipal.toString() === campaign.owner.toString();
  };

  const isCurrentUser = (participantPrincipal: Principal) => {
    return participantPrincipal.toString() === currentUserPrincipal?.toString();
  };

  const canRemove = (participantPrincipal: Principal) => {
    // Only campaign owners can remove participants
    // Cannot remove the campaign owner themselves
    // Cannot remove yourself (current user)
    return isOwner && 
           !isOwnerParticipant(participantPrincipal) && 
           !isCurrentUser(participantPrincipal) &&
           currentUserPrincipal?.toString() === campaign.owner.toString();
  };

  const canEditRole = (participantPrincipal: Principal) => {
    // Only campaign owners can edit roles
    // Campaign owners can now edit their own roles as well as other participants' roles
    return isOwner && 
           currentUserPrincipal?.toString() === campaign.owner.toString();
  };

  const canLeaveCampaign = () => {
    // Only non-owners can leave campaigns
    return !isOwner && 
           currentUserPrincipal && 
           !isOwnerParticipant(currentUserPrincipal);
  };

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="bg-gray-900 border-white/20 text-white max-w-2xl max-h-[80vh]">
          <DialogHeader className="pb-4">
            <DialogTitle className="flex items-center gap-2 text-xl">
              {isOwner ? <Users className="h-5 w-5 text-blue-400" /> : <Eye className="h-5 w-5 text-blue-400" />}
              Campaign Participants
            </DialogTitle>
            <DialogDescription className="text-purple-200">
              {isOwner ? `Manage participants for "${campaign.name}"` : `View participants in "${campaign.name}"`}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold text-white">
                Participants ({participants.length})
              </h3>
              <div className="flex items-center space-x-2">
                {canLeaveCampaign() && (
                  <Button
                    onClick={handleLeaveCampaignClick}
                    variant="outline"
                    size="sm"
                    className="border-red-400/30 text-red-400 hover:bg-red-500/10 hover:border-red-400"
                  >
                    <LogOut className="h-4 w-4 mr-2" />
                    Leave Campaign
                  </Button>
                )}
                {isOwner && (
                  <Button
                    onClick={() => setIsInviteDialogOpen(true)}
                    className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
                    size="sm"
                  >
                    <UserPlus className="h-4 w-4 mr-2" />
                    Invite Player
                  </Button>
                )}
              </div>
            </div>

            <Separator className="bg-white/20" />

            <ScrollArea className="h-[400px] pr-4">
              {loadingParticipants ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="h-8 w-8 animate-spin text-purple-400" />
                  <span className="ml-2 text-purple-200">Loading participants...</span>
                </div>
              ) : participants.length === 0 ? (
                <div className="text-center py-8">
                  <Users className="h-12 w-12 text-purple-400 mx-auto mb-4" />
                  <p className="text-purple-200 mb-2">No participants yet</p>
                  {isOwner ? (
                    <p className="text-sm text-purple-300">Invite players to join your campaign</p>
                  ) : (
                    <p className="text-sm text-purple-300">This campaign has no participants</p>
                  )}
                </div>
              ) : (
                <div className="space-y-3">
                  {participants.map((participant) => (
                    <div
                      key={participant.principal.toString()}
                      className="flex items-center justify-between p-4 bg-white/5 rounded-lg border border-white/10 hover:bg-white/10 transition-colors"
                    >
                      <div className="flex items-center space-x-3 flex-1 min-w-0">
                        <div className="flex items-center space-x-2">
                          {getRoleIcon(participant.role)}
                          <div className="min-w-0 flex-1">
                            <p className="font-medium text-white truncate">
                              {participant.name || 'Unknown User'}
                              {isOwnerParticipant(participant.principal) && (
                                <span className="ml-2 text-xs text-amber-400 font-normal">(Owner)</span>
                              )}
                              {isCurrentUser(participant.principal) && !isOwnerParticipant(participant.principal) && (
                                <span className="ml-2 text-xs text-blue-400 font-normal">(You)</span>
                              )}
                              {isCurrentUser(participant.principal) && isOwnerParticipant(participant.principal) && (
                                <span className="ml-2 text-xs text-amber-400 font-normal">(You - Owner)</span>
                              )}
                            </p>
                            <p className="text-xs text-purple-300 truncate">
                              {participant.principal.toString().slice(0, 20)}...
                            </p>
                          </div>
                        </div>
                      </div>
                      <div className="flex items-center space-x-2 flex-shrink-0">
                        {canEditRole(participant.principal) ? (
                          <Select
                            value={participant.role}
                            onValueChange={(newRole: Role) => handleRoleChange(participant, newRole)}
                            disabled={updateParticipantRole.isPending}
                          >
                            <SelectTrigger className="w-40 h-8 bg-white/10 border-white/20 text-white text-xs">
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent 
                              className="bg-gray-900 border-white/20 min-w-[180px]" 
                              side="left"
                              align="start"
                              sideOffset={10}
                            >
                              <SelectItem value={Role.player} className="text-white hover:bg-white/10">
                                <div className="flex items-center space-x-2">
                                  <Gamepad2 className="h-3 w-3 text-blue-400" />
                                  <span>Player</span>
                                </div>
                              </SelectItem>
                              <SelectItem value={Role.gm} className="text-white hover:bg-white/10">
                                <div className="flex items-center space-x-2">
                                  <Crown className="h-3 w-3 text-amber-400" />
                                  <span>GM</span>
                                </div>
                              </SelectItem>
                              <SelectItem value={Role.both} className="text-white hover:bg-white/10">
                                <div className="flex items-center space-x-2">
                                  <Crown className="h-3 w-3 text-amber-400" />
                                  <Gamepad2 className="h-3 w-3 text-blue-400" />
                                  <span>GM & Player</span>
                                </div>
                              </SelectItem>
                            </SelectContent>
                          </Select>
                        ) : (
                          <Badge
                            variant={getRoleBadgeVariant(participant.role)}
                            className="text-xs whitespace-nowrap"
                          >
                            {getRoleText(participant.role)}
                          </Badge>
                        )}
                        {canRemove(participant.principal) && (
                          <Button
                            onClick={() => handleRemoveParticipantClick(participant)}
                            variant="ghost"
                            size="sm"
                            className="h-8 w-8 p-0 text-red-400 hover:text-red-300 hover:bg-red-500/10 transition-colors"
                            disabled={removeParticipant.isPending}
                            title={`Remove ${participant.name || 'participant'} from campaign`}
                          >
                            <UserX className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </ScrollArea>
          </div>
        </DialogContent>
      </Dialog>

      {/* Mandatory Participant Removal Confirmation Modal */}
      <AlertDialog open={isRemoveConfirmOpen} onOpenChange={setIsRemoveConfirmOpen}>
        <AlertDialogContent className="bg-gray-900 border-red-400/30 text-white max-w-md">
          <AlertDialogHeader className="space-y-4">
            <div className="flex items-center justify-center">
              <div className="bg-red-500/20 p-3 rounded-full">
                <AlertTriangle className="h-8 w-8 text-red-400" />
              </div>
            </div>
            <AlertDialogTitle className="text-center text-xl font-bold text-white">
              Remove Participant
            </AlertDialogTitle>
            <AlertDialogDescription className="text-center text-purple-200 space-y-2">
              <p className="font-medium">
                Are you sure you want to remove <span className="text-white font-semibold">"{participantToRemove?.name || 'Unknown User'}"</span> from the campaign?
              </p>
              <p className="text-sm">
                <span className="font-medium text-amber-300">Campaign:</span> {campaign.name}
              </p>
              <p className="text-sm text-red-300">
                This action cannot be undone. The participant will lose access to the campaign immediately.
              </p>
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="flex-col sm:flex-row gap-2 sm:gap-0">
            <AlertDialogCancel 
              onClick={handleCancelRemoveParticipant}
              className="bg-white/10 border-white/20 text-white hover:bg-white/20 order-2 sm:order-1"
              disabled={removeParticipant.isPending}
            >
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmRemoveParticipant}
              disabled={removeParticipant.isPending}
              className="bg-red-600 hover:bg-red-700 text-white border-0 order-1 sm:order-2"
            >
              {removeParticipant.isPending ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Removing...
                </>
              ) : (
                <>
                  <UserX className="h-4 w-4 mr-2" />
                  Confirm Removal
                </>
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Leave Campaign Confirmation Modal */}
      <AlertDialog open={isLeaveConfirmOpen} onOpenChange={setIsLeaveConfirmOpen}>
        <AlertDialogContent className="bg-gray-900 border-orange-400/30 text-white max-w-md">
          <AlertDialogHeader className="space-y-4">
            <div className="flex items-center justify-center">
              <div className="bg-orange-500/20 p-3 rounded-full">
                <LogOut className="h-8 w-8 text-orange-400" />
              </div>
            </div>
            <AlertDialogTitle className="text-center text-xl font-bold text-white">
              Leave Campaign
            </AlertDialogTitle>
            <AlertDialogDescription className="text-center text-purple-200 space-y-2">
              <p className="font-medium">
                Are you sure you want to leave <span className="text-white font-semibold">"{campaign.name}"</span>?
              </p>
              <p className="text-sm text-orange-300">
                You will lose access to this campaign immediately. You can only rejoin if the campaign owner invites you again.
              </p>
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="flex-col sm:flex-row gap-2 sm:gap-0">
            <AlertDialogCancel 
              onClick={handleCancelLeaveCampaign}
              className="bg-white/10 border-white/20 text-white hover:bg-white/20 order-2 sm:order-1"
              disabled={leaveCampaign.isPending}
            >
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmLeaveCampaign}
              disabled={leaveCampaign.isPending}
              className="bg-orange-600 hover:bg-orange-700 text-white border-0 order-1 sm:order-2"
            >
              {leaveCampaign.isPending ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Leaving...
                </>
              ) : (
                <>
                  <LogOut className="h-4 w-4 mr-2" />
                  Leave Campaign
                </>
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {isOwner && (
        <SendInvitationDialog
          campaign={campaign}
          open={isInviteDialogOpen}
          onOpenChange={setIsInviteDialogOpen}
          onInviteSent={handleInviteSent}
        />
      )}
    </>
  );
}
