import React, { useState, useEffect } from 'react';
import { Session, Participant, Role, SessionPlayer } from '../backend';
import { useGetSessionPlayers, useAddSessionPlayer, useRemoveSessionPlayer, useGetUserProfile } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { Users, UserPlus, UserX, Loader2, Eye, AlertTriangle, Crown, Gamepad2 } from 'lucide-react';
import { Principal } from '@dfinity/principal';

interface SessionPlayersDialogProps {
  session: Session;
  campaignParticipants: Array<Participant & { name?: string }>;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSessionPlayersUpdated?: () => void;
}

interface SessionPlayerWithProfile extends SessionPlayer {
  name?: string;
}

export default function SessionPlayersDialog({ 
  session, 
  campaignParticipants, 
  open, 
  onOpenChange, 
  onSessionPlayersUpdated 
}: SessionPlayersDialogProps) {
  const [sessionPlayers, setSessionPlayers] = useState<SessionPlayerWithProfile[]>([]);
  const [loadingSessionPlayers, setLoadingSessionPlayers] = useState(false);
  const [isRemoveConfirmOpen, setIsRemoveConfirmOpen] = useState(false);
  const [playerToRemove, setPlayerToRemove] = useState<SessionPlayerWithProfile | null>(null);
  const [selectedParticipant, setSelectedParticipant] = useState<string>('');
  
  const getSessionPlayers = useGetSessionPlayers();
  const addSessionPlayer = useAddSessionPlayer();
  const removeSessionPlayer = useRemoveSessionPlayer();
  const getUserProfile = useGetUserProfile();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();
  const isSessionOwner = session.creator.toString() === currentUserPrincipal?.toString();

  // Get eligible participants (those with Player role or both Player and GM roles)
  const eligibleParticipants = campaignParticipants.filter(p => 
    p.role === Role.player || p.role === Role.both
  );

  // Get participants not already in the session
  const availableParticipants = eligibleParticipants.filter(p => 
    !sessionPlayers.some(sp => sp.participant.toString() === p.principal.toString())
  );

  useEffect(() => {
    if (open) {
      loadSessionPlayers();
    }
  }, [open]);

  const loadSessionPlayers = async () => {
    setLoadingSessionPlayers(true);
    try {
      const playerList = await getSessionPlayers.mutateAsync(session.id);
      const playersWithProfiles = await Promise.all(
        playerList.map(async (player) => {
          try {
            const profile = await getUserProfile.mutateAsync(player.participant);
            return {
              ...player,
              name: profile?.name,
            };
          } catch {
            return player;
          }
        })
      );
      setSessionPlayers(playersWithProfiles);
    } catch (error) {
      console.error('Failed to load session players:', error);
    } finally {
      setLoadingSessionPlayers(false);
    }
  };

  const handleAddSessionPlayer = async () => {
    if (!selectedParticipant) return;

    const participant = Principal.fromText(selectedParticipant);
    try {
      await addSessionPlayer.mutateAsync({
        sessionId: session.id,
        participant,
      });
      await loadSessionPlayers();
      setSelectedParticipant('');
      onSessionPlayersUpdated?.();
    } catch (error) {
      console.error('Failed to add session player:', error);
    }
  };

  const handleRemoveSessionPlayerClick = (player: SessionPlayerWithProfile) => {
    setPlayerToRemove(player);
    setIsRemoveConfirmOpen(true);
  };

  const handleConfirmRemoveSessionPlayer = async () => {
    if (!playerToRemove) return;

    try {
      await removeSessionPlayer.mutateAsync({
        sessionId: session.id,
        participant: playerToRemove.participant,
      });
      await loadSessionPlayers();
      setIsRemoveConfirmOpen(false);
      setPlayerToRemove(null);
      onSessionPlayersUpdated?.();
    } catch (error) {
      console.error('Failed to remove session player:', error);
    }
  };

  const handleCancelRemoveSessionPlayer = () => {
    setIsRemoveConfirmOpen(false);
    setPlayerToRemove(null);
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

  const getParticipantRole = (participantPrincipal: Principal) => {
    const participant = campaignParticipants.find(p => 
      p.principal.toString() === participantPrincipal.toString()
    );
    return participant?.role;
  };

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="bg-gray-900 border-white/20 text-white max-w-2xl max-h-[80vh]">
          <DialogHeader className="pb-4">
            <DialogTitle className="flex items-center gap-2 text-xl">
              {isSessionOwner ? <Users className="h-5 w-5 text-blue-400" /> : <Eye className="h-5 w-5 text-blue-400" />}
              Session Players
            </DialogTitle>
            <DialogDescription className="text-purple-200">
              {isSessionOwner ? `Manage players for "${session.name}"` : `View players in "${session.name}"`}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            {isSessionOwner && availableParticipants.length > 0 && (
              <div className="space-y-3">
                <h4 className="text-sm font-medium text-purple-300">Add Session Player</h4>
                <div className="flex items-center space-x-2">
                  <Select value={selectedParticipant} onValueChange={setSelectedParticipant}>
                    <SelectTrigger className="flex-1 bg-white/10 border-white/20 text-white">
                      <SelectValue placeholder="Select a participant to add" />
                    </SelectTrigger>
                    <SelectContent className="bg-gray-900 border-white/20">
                      {availableParticipants.map((participant) => (
                        <SelectItem 
                          key={participant.principal.toString()} 
                          value={participant.principal.toString()}
                          className="text-white hover:bg-white/10"
                        >
                          <div className="flex items-center space-x-2">
                            {getRoleIcon(participant.role)}
                            <span>{participant.name || 'Unknown User'}</span>
                          </div>
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <Button
                    onClick={handleAddSessionPlayer}
                    disabled={!selectedParticipant || addSessionPlayer.isPending}
                    className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0"
                  >
                    {addSessionPlayer.isPending ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      <UserPlus className="h-4 w-4" />
                    )}
                  </Button>
                </div>
                <Separator className="bg-white/20" />
              </div>
            )}

            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold text-white">
                Session Players ({sessionPlayers.length})
              </h3>
            </div>

            <ScrollArea className="h-[400px] pr-4">
              {loadingSessionPlayers ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="h-8 w-8 animate-spin text-purple-400" />
                  <span className="ml-2 text-purple-200">Loading session players...</span>
                </div>
              ) : sessionPlayers.length === 0 ? (
                <div className="text-center py-8">
                  <Users className="h-12 w-12 text-purple-400 mx-auto mb-4" />
                  <h4 className="text-lg font-semibold text-white mb-2">No Session Players</h4>
                  <p className="text-purple-200 mb-4 text-sm">
                    {isSessionOwner 
                      ? "Add players to this session to track who participated."
                      : "No players have been added to this session yet."
                    }
                  </p>
                </div>
              ) : (
                <div className="space-y-3">
                  {sessionPlayers.map((player) => {
                    const role = getParticipantRole(player.participant);
                    return (
                      <div
                        key={player.participant.toString()}
                        className="flex items-center justify-between p-4 bg-white/5 rounded-lg border border-white/10 hover:bg-white/10 transition-colors"
                      >
                        <div className="flex items-center space-x-3 flex-1 min-w-0">
                          <div className="flex items-center space-x-2">
                            {role && getRoleIcon(role)}
                            <div className="min-w-0 flex-1">
                              <p className="font-medium text-white truncate">
                                {player.name || 'Unknown User'}
                              </p>
                              <p className="text-xs text-purple-300 truncate">
                                {player.participant.toString().slice(0, 20)}...
                              </p>
                            </div>
                          </div>
                        </div>
                        <div className="flex items-center space-x-2 flex-shrink-0">
                          <Badge variant="outline" className="text-xs text-blue-300 border-blue-400/30">
                            Session Player
                          </Badge>
                          {isSessionOwner && (
                            <Button
                              onClick={() => handleRemoveSessionPlayerClick(player)}
                              variant="ghost"
                              size="sm"
                              className="h-8 w-8 p-0 text-red-400 hover:text-red-300 hover:bg-red-500/10 transition-colors"
                              disabled={removeSessionPlayer.isPending}
                              title={`Remove ${player.name || 'player'} from session`}
                            >
                              <UserX className="h-4 w-4" />
                            </Button>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </ScrollArea>
          </div>
        </DialogContent>
      </Dialog>

      {/* Session Player Removal Confirmation Modal */}
      <AlertDialog open={isRemoveConfirmOpen} onOpenChange={setIsRemoveConfirmOpen}>
        <AlertDialogContent className="bg-gray-900 border-red-400/30 text-white max-w-md">
          <AlertDialogHeader className="space-y-4">
            <div className="flex items-center justify-center">
              <div className="bg-red-500/20 p-3 rounded-full">
                <AlertTriangle className="h-8 w-8 text-red-400" />
              </div>
            </div>
            <AlertDialogTitle className="text-center text-xl font-bold text-white">
              Remove Session Player
            </AlertDialogTitle>
            <AlertDialogDescription className="text-center text-purple-200 space-y-2">
              <p className="font-medium">
                Are you sure you want to remove <span className="text-white font-semibold">"{playerToRemove?.name || 'Unknown User'}"</span> from this session?
              </p>
              <p className="text-sm">
                <span className="font-medium text-blue-300">Session:</span> {session.name}
              </p>
              <p className="text-sm text-red-300">
                This action cannot be undone. The player will be removed from this session.
              </p>
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="flex-col sm:flex-row gap-2 sm:gap-0">
            <AlertDialogCancel 
              onClick={handleCancelRemoveSessionPlayer}
              className="bg-white/10 border-white/20 text-white hover:bg-white/20 order-2 sm:order-1"
              disabled={removeSessionPlayer.isPending}
            >
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmRemoveSessionPlayer}
              disabled={removeSessionPlayer.isPending}
              className="bg-red-600 hover:bg-red-700 text-white border-0 order-1 sm:order-2"
            >
              {removeSessionPlayer.isPending ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Removing...
                </>
              ) : (
                <>
                  <UserX className="h-4 w-4 mr-2" />
                  Remove Player
                </>
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}
